# Creates the GELF TCP input required by AMMS.Api (Serilog transportType=Tcp).
# Run after a fresh Graylog setup or when only GELF UDP AMMS exists.
# Usage: .\setup-graylog-tcp-input.ps1 [-GraylogUrl http://localhost:9000] [-Username admin] [-Password admin]

param(
    [string]$GraylogUrl = "http://localhost:9000",
    [string]$Username = "admin",
    [string]$Password = "admin"
)

$ErrorActionPreference = "Stop"

$authBytes = [System.Text.Encoding]::ASCII.GetBytes("${Username}:${Password}")
$headers = @{
    Authorization = "Basic $([System.Convert]::ToBase64String($authBytes))"
    "X-Requested-By" = "amms-setup"
}

$inputs = Invoke-RestMethod -Uri "$GraylogUrl/api/system/inputs" -Headers $headers
$existing = $inputs.inputs | Where-Object { $_.title -eq "GELF TCP AMMS" }

if ($existing) {
    Write-Host "GELF TCP AMMS input already exists (id=$($existing.id), port=$($existing.attributes.port))."
    exit 0
}

$body = @{
    title = "GELF TCP AMMS"
    type = "org.graylog2.inputs.gelf.tcp.GELFTCPInput"
    global = $true
    configuration = @{
        bind_address = "0.0.0.0"
        port = 12201
        recv_buffer_size = 262144
        tcp_keepalive = $false
        use_null_delimiter = $true
        max_message_size = 2097152
        decompress_size_limit = 8388608
    }
} | ConvertTo-Json -Depth 5

$result = Invoke-RestMethod -Method Post -Uri "$GraylogUrl/api/system/inputs" -Headers $headers -ContentType "application/json" -Body $body
Write-Host "Created GELF TCP AMMS input (id=$($result.id), port=12201)."
