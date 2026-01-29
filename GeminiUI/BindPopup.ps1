$baseUri = "http://127.0.0.1:8081/";

function Send-UnityCommand {
    param (
        [string]$CommandType,
        [object]$CommandArgs
    )
    $argsJson = ConvertTo-Json $CommandArgs -Compress
    # Escape quotes for placing inside a JSON string
    $argsJsonEscaped = $argsJson.Replace('"', '\"')
    
    $body = @{
        CommandType = $CommandType
        ArgsJson = $argsJson
    } | ConvertTo-Json
    
    try {
        Write-Host "Sending Body: $body"
        $response = Invoke-RestMethod -Uri $baseUri -Method Post -ContentType "application/json" -Body $body
        Write-Host "Response: $($response | ConvertTo-Json -Depth 5)"
    } catch {
        Write-Host "Error: $_"
        if ($_.Exception.Response) {
             $reader = New-Object System.IO.StreamReader $_.Exception.Response.GetResponseStream()
             Write-Host "Server Error Detail: $($reader.ReadToEnd())"
        }
    }
}

$prefab = "Assets/Prefabs/TwinImageTestPopup.prefab"

Write-Host "Binding Duck Sprite..."
Send-UnityCommand -CommandType "unity_bind_component" -CommandArgs @{
    prefabPath = $prefab
    uiElementName = "Window/ImageRow/DuckImage"
    scriptName = "Image"
    fieldName = "m_Sprite"
    targetAssetPath = "Assets/Textures/Duck.png"
}

Write-Host "Binding Goose Sprite..."
Send-UnityCommand -CommandType "unity_bind_component" -CommandArgs @{
    prefabPath = $prefab
    uiElementName = "Window/ImageRow/GooseImage"
    scriptName = "Image"
    fieldName = "m_Sprite"
    targetAssetPath = "Assets/Textures/Goose.png"
}

Write-Host "Binding Logic LeftImage..."
Send-UnityCommand -CommandType "unity_bind_component" -CommandArgs @{
    prefabPath = $prefab
    uiElementName = "Window/Logic"
    scriptName = "TwinImagePopup"
    fieldName = "leftImage"
    targetElementName = "Window/ImageRow/DuckImage"
}

Write-Host "Binding Logic RightImage..."
Send-UnityCommand -CommandType "unity_bind_component" -CommandArgs @{
    prefabPath = $prefab
    uiElementName = "Window/Logic"
    scriptName = "TwinImagePopup"
    fieldName = "rightImage"
    targetElementName = "Window/ImageRow/GooseImage"
}

Write-Host "Binding Logic CloseButton..."
Send-UnityCommand -CommandType "unity_bind_component" -CommandArgs @{
    prefabPath = $prefab
    uiElementName = "Window/Logic"
    scriptName = "TwinImagePopup"
    fieldName = "closeButton"
    targetElementName = "Window/CloseButton"
}
