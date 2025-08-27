param (
    [string]$location = "swedencentral",
    [string]$prefix = "projectborealis"
)

# If the below fails,
# az extension add --name application-insights
# then run this first,
# $ENV:PYTHONPATH = "C:\\Program Files\\Microsoft SDKs\\Azure\\CLI2"

az group create --name "${prefix}-rg" --location $location

az storage account create --name "${prefix}st" --location $location --resource-group "${prefix}-rg" --sku "Standard_LRS" --allow-blob-public-access false --allow-shared-key-access false

$output=$(az identity create --name "${prefix}-func-host-storage-user" --resource-group "${prefix}-rg" --location $location --query "{userId:id, principalId: principalId, clientId: clientId}" -o json)

$userId=$(echo $output | jq -r '.userId')
$principalId=$(echo $output | jq -r '.principalId')
$clientId=$(echo $output | jq -r '.clientId')

$storageId=$(az storage account show --resource-group "${prefix}-rg" --name "${prefix}st" --query 'id' -o tsv)
az role assignment create --assignee-object-id $principalId --assignee-principal-type ServicePrincipal --role "Storage Blob Data Owner" --scope $storageId

az functionapp create --resource-group "${prefix}-rg" --name "${prefix}-func-app" --flexconsumption-location $location --runtime dotnet-isolated --runtime-version "9" --storage-account "${prefix}st" --deployment-storage-auth-type UserAssignedIdentity --deployment-storage-auth-value "${prefix}-func-host-storage-user" --instance-memory 512 --maximum-instance-count 1

$appInsights=$(az monitor app-insights component show --resource-group "${prefix}-rg" --app "${prefix}-func-app" --query "id" --output tsv)
az role assignment create --role "Monitoring Metrics Publisher" --assignee $principalId --scope $appInsights

az functionapp config appsettings set --name "${prefix}-func-app" --resource-group "${prefix}-rg" --settings AzureWebJobsStorage__accountName="${prefix}st" AzureWebJobsStorage__credential=managedidentity AzureWebJobsStorage__clientId=$clientId APPLICATIONINSIGHTS_AUTHENTICATION_STRING="ClientId=$clientId;Authorization=AAD"

az functionapp config appsettings delete --name "${prefix}-func-app" --resource-group "${prefix}-rg" --setting-names AzureWebJobsStorage
