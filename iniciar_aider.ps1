# 1. Configura tu API Key de OpenRouter
$env:OPENROUTER_API_KEY = "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"

# 2. El modelo que analizamos originalmente para C# y Blazor
$MODELO_GRATUITO = "openrouter/openrouter/free"

Clear-Host
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "  Iniciando Aider AI Pair Programming (.NET / Blazor)     " -ForegroundColor Green
Write-Host "  Modelo: $MODELO_GRATUITO" -ForegroundColor Yellow
Write-Host "==========================================================" -ForegroundColor Cyan

# 3. Ejecución de Aider con auto-compilación
aider --model $MODELO_GRATUITO --auto-lint --lint-cmd "dotnet build"