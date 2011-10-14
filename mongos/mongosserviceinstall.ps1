$srvanyExe = "srvany.exe"
$serviceName = "MongoS"
$serviceDisplayName = "MongoDB Router"
$serviceDesc = "Service that runs the MongoDB Router mongos"
$serviceRegPath = Join-Path "HKLM:\SYSTEM\CurrentControlSet\Services\" $serviceName

function Get-SrvanyPath {
    $currentPath = Resolve-Path .
    $exePath = Join-Path $currentPath $srvanyExe
    
    if (Test-Path -LiteralPath $exePath -PathType Leaf) {
        return $exePath
    }

    $progFilesPath = [Environment]::GetEnvironmentVariable("ProgramFiles(x86)")
    if ($progFilesPath -eq $null) {
        $progFilesPath = [Environment]::GetEnvironmentVariable("ProgramFiles")
    }
    $exePath = Join-Path $progFilesPath (Join-Path "\Windows Resource Kits\Tools" $srvanyExe)
    if (Test-Path -LiteralPath $exePath -PathType Leaf) {
        return $exePath
    }
    else {
        Write-Error "Error: Windows Resource Toolkit srvany not found" `
            -Category NotInstalled
        break
    }
}

function Create-MongosService {
    Param($execName)
    $svcList = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
    if ($svcList -eq $null -or $svcList.Count -eq 0) {
        New-Service -Name $serviceName -BinaryPathName $execName `
            -Description $serviceDesc -DisplayName $serviceDisplayName `
            -StartupType Automatic | Out-Null
    }
    else {
        Write-Error "Service already exists" -Category ResourceExists
        break
    }
}

function Modify-MongosService {
    $mongosFullPath = Resolve-Path $mongos
    $quotedMongosPath = Quote-String($mongosFullPath)
    $appDir = Get-Item $mongosFullPath | Split-Path -parent
    $quotedAppDir = Quote-String($appDir)
    $quotedConfigPath = Quote-String(Resolve-Path $configFile)
    $application = $quotedMongosPath+" --config "+$quotedConfigPath
    $parameterKey = Join-Path $serviceRegPath "Parameters"
    New-Item -Path  $parameterKey | Out-Null
    New-ItemProperty -Path $parameterKey -Name "Application" -Value $application | Out-Null
    New-ItemProperty -Path $parameterKey -Name "AppDir" -Value $quotedAppDir | Out-Null
}

function Start-MongosService {
    Start-Service -Name $serviceName
}

function Quote-String {
    Param($unquotedString)
    return "`"" + $unquotedString + "`""
}  

function Check-ConfigFile {
    Param($location)
    if (Test-Path -Path $location -PathType Leaf) {
        #do nothing
    }
    else {
        Write-Error -Message "Config file path not found. Service not installed" `
            -Category InvalidArgument
        Exit
    }
}

function Check-Mongos {
    Param($location)
    if ($location -eq $null) {
        $location = "."
    }
    if (Test-Path -LiteralPath $location -PathType Leaf) {
        #do nothing
    }
    else {
        if (Test-Path -Path $location -PathType Container) {
            $mongos = Join-Path $location "mongos.exe"
            if (Test-Path -LiteralPath $mongos -PathType Leaf) {
                # do nothing
            }
            else {
                Write-Error -Message "Config file path not specified. Service not installed" `
                    -Category InvalidArgument
                Exit
            }
        }
    }
}
    
if ($args.Length -eq 0) {
    Write-Error -Message "Arguments not specified" `
        -Category InvalidArgument
    Write-Host "Usage: powershell mongosserviceinstall.ps1 <config file path> <mongos path>"
    Exit
}
elseif ($args.Length -eq 1) {
    $configFile = $args[0]
}
else {
    $configFile = $args[0]
    $mongos = $args[1]
}
   
Check-ConfigFile $configFile
Check-Mongos $mongos
$srvAnyPath = Get-SrvanyPath

Create-MongosService(Quote-String($srvAnyPath))
Modify-MongosService
Write-Host "Service succesfully created"
Start-MongosService
Write-Host "Service started"