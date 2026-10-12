# =====================================================================
#  EstoqueFácil - publicar na VPS (rodar no Windows, no PowerShell)
#  Uso:  .\publicar.ps1 -Servidor 123.45.67.89            (usuário root)
#        .\publicar.ps1 -Servidor 123.45.67.89 -Usuario ubuntu
#  Pré-requisitos no PC: .NET 10 SDK, Node/npm, dotnet-ef, OpenSSH (nativo no Windows 10/11)
# =====================================================================
param(
  [Parameter(Mandatory = $true)][string]$Servidor,
  [string]$Usuario = "root",
  [string]$BackEnd  = "C:\Pasta_Dev\BackEnd\EstoqueFacilBackEnd",
  [string]$FrontEnd = "C:\Pasta_Dev\FrontEnd\EstoqueFacilFrontEnd"
)
$ErrorActionPreference = "Stop"
$destino = "$Usuario@$Servidor"
$tmp = Join-Path $env:TEMP "estoquefacil-deploy"
Remove-Item $tmp -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $tmp | Out-Null

Write-Host ">> 1/5 Publicando API (.NET, linux-x64, self-contained)..." -ForegroundColor Cyan
dotnet publish "$BackEnd\EstoqueFacil.API\EstoqueFacil.API.csproj" -c Release -r linux-x64 --self-contained true -o "$tmp\api"
if ($LASTEXITCODE -ne 0) { throw "Falha no dotnet publish" }

Write-Host ">> 2/5 Gerando script de migrations (idempotente)..." -ForegroundColor Cyan
if (-not (Get-Command dotnet-ef -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-ef }
dotnet ef migrations script --idempotent -o "$tmp\migrations.sql" `
  --project "$BackEnd\EstoqueFacil.Infrastructure" --startup-project "$BackEnd\EstoqueFacil.API"
if ($LASTEXITCODE -ne 0) { throw "Falha ao gerar migrations.sql" }

Write-Host ">> 3/5 Build do Angular (produção)..." -ForegroundColor Cyan
Push-Location $FrontEnd
npm ci
npx ng build --configuration production
if ($LASTEXITCODE -ne 0) { Pop-Location; throw "Falha no ng build" }
Pop-Location
$web = "$FrontEnd\dist\EstoqueFacilFrontEnd\browser"
Copy-Item $web "$tmp\web" -Recurse
if (-not (Test-Path "$tmp\web\index.html") -and (Test-Path "$tmp\web\index.csr.html")) {
  Copy-Item "$tmp\web\index.csr.html" "$tmp\web\index.html"
}

Write-Host ">> 4/5 Enviando arquivos para $destino ..." -ForegroundColor Cyan
tar -czf "$tmp\pacote.tgz" -C $tmp api web migrations.sql
scp "$tmp\pacote.tgz" "${destino}:/tmp/estoquefacil-pacote.tgz"
if ($LASTEXITCODE -ne 0) { throw "Falha no scp" }

Write-Host ">> 5/5 Instalando no servidor..." -ForegroundColor Cyan
$remoto = @'
set -e
SUDO="sudo"; [ "$(id -u)" -eq 0 ] && SUDO=""
D=/tmp/estoquefacil-novo
rm -rf $D && mkdir -p $D && tar -xzf /tmp/estoquefacil-pacote.tgz -C $D
$SUDO systemctl stop estoquefacil-api || true
chmod -R a+rX $D
runas() { if [ -z "$SUDO" ]; then runuser -u postgres -- "$@"; else sudo -u postgres "$@"; fi; }
runas psql -d estoque_facil_db -v ON_ERROR_STOP=1 -c "SET ROLE estoquefacil" -f $D/migrations.sql
$SUDO rm -rf /var/www/estoquefacil/api/* /var/www/estoquefacil/web/*
$SUDO cp -r $D/api/. /var/www/estoquefacil/api/
$SUDO cp -r $D/web/. /var/www/estoquefacil/web/
$SUDO chmod +x /var/www/estoquefacil/api/EstoqueFacil.API
$SUDO chown -R estoquefacil:estoquefacil /var/www/estoquefacil/api
$SUDO systemctl start estoquefacil-api
sleep 3
$SUDO systemctl --no-pager --lines=15 status estoquefacil-api
rm -rf $D /tmp/estoquefacil-pacote.tgz
'@
$remoto = $remoto -replace "`r`n", "`n"
$remoto | ssh $destino "bash -s"
if ($LASTEXITCODE -ne 0) { throw "Falha na instalação remota" }

Write-Host ""
Write-Host "Publicado! Acesse: https://estoquefacil.bmgteh.com.br" -ForegroundColor Green
