param (
    [string]$prefix = "projectborealis"
)

func azure functionapp publish "${prefix}-func-app"
