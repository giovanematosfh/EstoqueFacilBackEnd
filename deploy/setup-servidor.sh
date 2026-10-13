#!/usr/bin/env bash
# =====================================================================
#  EstoqueFácil - preparação da VPS (rodar UMA vez, como root)
#  Uso:  sudo bash setup-servidor.sh
#  Testado para Ubuntu 22.04 / 24.04 e Debian 12.
# =====================================================================
set -euo pipefail

DOMINIO="estoquefacil.bmgteh.com.br"
APP_DIR="/var/www/estoquefacil"
API_PORT="5102"
DB_NAME="estoque_facil_db"
DB_USER="estoquefacil"
ENV_FILE="/etc/estoquefacil/api.env"

read -rp "E-mail para o certificado SSL (Let's Encrypt): " CERT_EMAIL

echo ">> Instalando pacotes (nginx, postgresql, certbot)..."
apt-get update -y
DEBIAN_FRONTEND=noninteractive apt-get install -y nginx postgresql certbot python3-certbot-nginx openssl

echo ">> Criando usuário de sistema e pastas..."
id -u estoquefacil &>/dev/null || useradd --system --no-create-home --shell /usr/sbin/nologin estoquefacil
mkdir -p "$APP_DIR/api" "$APP_DIR/web" /etc/estoquefacil
chown -R estoquefacil:estoquefacil "$APP_DIR/api"

echo ">> Configurando PostgreSQL..."
if [ ! -f "$ENV_FILE" ]; then
  DB_PASS="$(openssl rand -hex 24)"
  JWT_KEY="$(openssl rand -hex 48)"
  sudo -u postgres psql -v ON_ERROR_STOP=1 <<SQL
DO \$\$ BEGIN
  IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = '${DB_USER}') THEN
    CREATE ROLE ${DB_USER} LOGIN PASSWORD '${DB_PASS}';
  ELSE
    ALTER ROLE ${DB_USER} WITH PASSWORD '${DB_PASS}';
  END IF;
END \$\$;
SQL
  sudo -u postgres psql -tc "SELECT 1 FROM pg_database WHERE datname='${DB_NAME}'" | grep -q 1 \
    || sudo -u postgres createdb -O "${DB_USER}" "${DB_NAME}"

  cat > "$ENV_FILE" <<ENV
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://127.0.0.1:${API_PORT}
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASS}
Jwt__Key=${JWT_KEY}
Frontend__BaseUrl=https://${DOMINIO}
# Preencha quando for ativar o envio de e-mail:
Smtp__Username=
Smtp__Password=
Smtp__FromEmail=
ENV
  chmod 600 "$ENV_FILE"
  echo "   Segredos gerados em $ENV_FILE"
else
  echo "   $ENV_FILE já existe - mantendo senhas atuais."
fi

echo ">> Criando serviço systemd da API..."
cat > /etc/systemd/system/estoquefacil-api.service <<UNIT
[Unit]
Description=EstoqueFacil API (.NET)
After=network.target postgresql.service

[Service]
WorkingDirectory=${APP_DIR}/api
ExecStart=${APP_DIR}/api/EstoqueFacil.API
EnvironmentFile=${ENV_FILE}
User=estoquefacil
Restart=always
RestartSec=5
KillSignal=SIGINT
SyslogIdentifier=estoquefacil-api

[Install]
WantedBy=multi-user.target
UNIT
systemctl daemon-reload
systemctl enable estoquefacil-api

echo ">> Configurando Nginx..."
cat > /etc/nginx/sites-available/estoquefacil <<NGINX
server {
    listen 80;
    listen [::]:80;
    server_name ${DOMINIO};

    root ${APP_DIR}/web;
    index index.html;
    client_max_body_size 20M;

    # API .NET
    location ^~ /api/ {
        proxy_pass         http://127.0.0.1:${API_PORT};
        proxy_http_version 1.1;
        proxy_set_header   Host \$host;
        proxy_set_header   X-Real-IP \$remote_addr;
        proxy_set_header   X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto \$scheme;
    }

    # Arquivos com hash do Angular: cache longo
    location ~* \.(?:js|css|woff2?|ttf|svg|png|jpg|jpeg|gif|ico|webp)\$ {
        try_files \$uri =404;
        expires 1y;
        add_header Cache-Control "public, immutable";
    }

    # SPA Angular: qualquer rota cai no index.html
    location / {
        try_files \$uri \$uri/ /index.html;
        add_header Cache-Control "no-cache";
    }
}
NGINX
ln -sf /etc/nginx/sites-available/estoquefacil /etc/nginx/sites-enabled/estoquefacil
nginx -t && systemctl reload nginx

if command -v ufw &>/dev/null && ufw status | grep -q "Status: active"; then
  ufw allow 'Nginx Full' || true
fi

echo ">> Emitindo certificado SSL..."
if certbot --nginx -d "$DOMINIO" --non-interactive --agree-tos -m "$CERT_EMAIL" --redirect; then
  echo "   HTTPS ativo."
else
  echo "   !! Não foi possível emitir o SSL agora (o DNS já aponta para este IP?)."
  echo "   Depois de ajustar o DNS rode:  certbot --nginx -d $DOMINIO --redirect"
fi

echo ""
echo "=== Servidor pronto. Agora rode o publicar.ps1 no seu computador. ==="
