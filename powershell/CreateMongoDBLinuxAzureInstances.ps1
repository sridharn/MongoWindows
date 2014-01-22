<#
 # file : CreateMongoDBLinuxAzureInstances.ps1
 # Licensed under the Apache License, Version 2.0 (the "License");
 # you may not use this file except in compliance with the License.
 # You may obtain a copy of the License at
 # 
 # 
 # http://www.apache.org/licenses/LICENSE-2.0
 # 
 # 
 # Unless required by applicable law or agreed to in writing, software
 # distributed under the License is distributed on an "AS IS" BASIS,
 # WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 # See the License for the specific language governing permissions and
 # limitations under the License.
 #>

<#
.SYNOPSIS 
Creates the specified number of Centos instances on Azure.

.DESCRIPTION
The script sets up the specified number Centos virtual machines in Azure under the same service. 
Each instance will have an SSH port that is provided by default. It also attaches 1 data disk 
per instance of size 127GB and creates a port per instance to use as the MongoDB port.
    
.PARAMETER Username
Username to create on the instances. All instances will have the same username.

.PARAMETER Password
Password for the user created on the instances. 

.PARAMETER ServiceName
Name of the service to which all instances belong. This will also be the DNS prefix of the instances

.PARAMETER Location
Data center to locate the service, virtual machines and data disk.

.PARAMETER InstanceCount
Number of instances to create. If it is not specified one instance is created

.INPUTS
None. You cannot pipe objects to CreateMongoDBLinuxAzureInstances.ps1.

.OUTPUTS
None. CreateMongoDBLinuxAzureInstances.ps1 does not generate any output.

.EXAMPLE
C:\PS> .\CreateMongoDBLinuxAzureInstances.ps1 myuser mypassword testservice "East US"
Deploy 1 instance to service "testservice" in the "East US" data center. The instance
will have 1 attached disk of size 127GB with the MongoDB port as 27017. 

.EXAMPLE
C:\PS> .\CreateMongoDBLinuxAzureInstances.ps1 myuser mypassword testservice "East US" 3
Deploy 4 instances to service "testservice" in the "East US" data center. Each of the 
instances will have 1 attached disk of size 127GB.The ports are 27017, 27018, 27019. 
#>

[CmdletBinding()]
Param(
    [Parameter(Mandatory=$True, Position=1)]
    [string]$UserName,
    
    [Parameter(Mandatory=$True, Position=2)]
    [string]$Password,
    
    [Parameter(Mandatory=$True, Position=3)]
    [string]$ServiceName,

    [Parameter(Mandatory=$True, Position=4)]
    [string]$Location,
    
    [Parameter(Mandatory=$False, Position=5)]
    [int]$InstanceCount
)

$imageName = "5112500ae3b842c8b9c604889f8753c3__OpenLogic-CentOS-63APR20130415"
$defaultDatadiskSize = 127
$defaultDatadiskLabel = "DataDisk"+$defaultDatadiskSize
$defaultBasePort = 27017


function CreateInstanceWithService {
    Param(
        [string]$userName,
        [string]$password,
        [string]$serviceName,
        [string]$location,
        [int]$instanceId)
    $instanceId=0
    $vmName = $serviceName+"_Mongo_"+$instanceId
    $port = $defaultBasePort + $instanceId
    New-AzureVMConfig -Name $vmName -InstanceSize ExtraLarge -ImageName $imageName `
        | Add-AzureProvisioningConfig -Linux -LinuxUser $userName -Password $password `
        | Add-AzureDataDisk -CreateNew -DiskSizeinGB $defaultDatadiskSize -DiskLabel $defaultDatadiskLabel -LUN 0 `
        | Add-AzureEndpoint -Name "MongoDB" -Protocol "tcp" -PublicPort $port -LocalPort $port `
        | New-AzureVM -ServiceName $serviceName -Location $location
}


function CreateInstance {
    Param(
        [string]$userName,
        [string]$password,
        [string]$serviceName,
        [string]$location,
        [int]$instanceId)
    $vmName = $serviceName+"_Mongo_"+$instanceId
    $port = $defaultBasePort + $instanceId
    New-AzureVMConfig -Name $vmName -InstanceSize ExtraLarge -ImageName $imageName `
        | Add-AzureProvisioningConfig -Linux -LinuxUser $userName -Password $password `
        | Add-AzureDataDisk -CreateNew -DiskSizeinGB $defaultDatadiskSize -DiskLabel $defaultDatadiskLabel -LUN 0 `
        | Add-AzureEndpoint -Name "MongoDB" -Protocol "tcp" -PublicPort $port -LocalPort $port `
        | New-AzureVM -ServiceName $serviceName   
}

function CreateInstances {
    Param(
        [string]$userName,
        [string]$password,
        [string]$serviceName,
        [string]$location,
        [int]$instanceCount)
    CreateInstanceWithService $userName $password $serviceName $location $instanceCount
    $instanceCounter = 1
    for (; $instanceCounter -lt $instanceCount; $instanceCounter++)
    {
        CreateInstance $userName $password $serviceName $location $instanceCounter
    }
}

CreateInstances $userName $password $serviceName $location $instanceCount
