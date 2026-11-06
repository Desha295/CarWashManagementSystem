param(
    [string]$Recipient = $env:BREVO_TEST_RECIPIENT
)

$apiKey = [Environment]::GetEnvironmentVariable('BREVO_API_KEY', 'User')
if ([string]::IsNullOrWhiteSpace($apiKey)) {
    Write-Error 'BREVO_API_KEY is not set in the current Windows user environment. Set it and start a new PowerShell window.'
    exit 1
}

try {
    Add-Type -AssemblyName System.Net.Http -ErrorAction Stop
} catch {
    Write-Error ("Could not load System.Net.Http: {0}" -f $_.Exception.Message)
    exit 1
}

if ([string]::IsNullOrWhiteSpace($Recipient)) {
    $Recipient = Read-Host 'Enter your email address for this test'
}
if ([string]::IsNullOrWhiteSpace($Recipient) -or $Recipient -notmatch '^[^\s@]+@[^\s@]+\.[^\s@]+$') {
    Write-Error 'Enter a valid recipient email address (or set BREVO_TEST_RECIPIENT).'
    exit 1
}

$httpClient = New-Object System.Net.Http.HttpClient
$request = [System.Net.Http.HttpRequestMessage]::new(
    [System.Net.Http.HttpMethod]::Post,
    'https://api.brevo.com/v3/smtp/email'
)
$request.Headers.Add('api-key', $apiKey)
$request.Headers.Add('accept', 'application/json')
$jsonBody = @{
        sender      = @{ name = 'Car Wash System'; email = 'asmae3629@gmail.com' }
        to          = @(@{ email = $Recipient })
        subject     = 'Car Wash System - API Test'
        textContent = "Hello!`nThis is a test email from Car Wash Management System.`nBrevo API integration is working successfully."
    } | ConvertTo-Json -Depth 6 -Compress
$request.Content = [System.Net.Http.StringContent]::new(
    $jsonBody,
    [System.Text.Encoding]::UTF8,
    'application/json'
)

try {
    $response = $httpClient.SendAsync($request).GetAwaiter().GetResult()
    $responseBody = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
    Write-Output ("HTTP {0} {1}" -f [int]$response.StatusCode, $response.ReasonPhrase)

    if ($response.IsSuccessStatusCode) {
        if (-not [string]::IsNullOrWhiteSpace($responseBody)) {
            Write-Output ("Brevo response: {0}" -f $responseBody)
            try {
                $parsed = $responseBody | ConvertFrom-Json
                if ($parsed.messageId) { Write-Output ("Message ID: {0}" -f $parsed.messageId) }
            } catch { }
        } else {
            Write-Output 'Brevo returned an empty response body.'
        }
    } else {
        Write-Output 'Brevo error response:'
        Write-Output $responseBody
        exit 1
    }
} catch {
    Write-Output ("Request failed: {0}" -f $_.Exception.Message)
    if ($_.Exception.InnerException) {
        Write-Output ("Details: {0}" -f $_.Exception.InnerException.Message)
    }
    exit 1
} finally {
    if ($response) { $response.Dispose() }
    $request.Dispose()
    $httpClient.Dispose()
}
