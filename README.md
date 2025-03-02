Repo to illustratte issue on Aspire : https://github.com/dotnet/aspire/discussions/7682

This is essentially the Aspire Starter project with two small mods : 

1) Add endpoint details and some relicas (>1) to the Api Service project reference :

<img width="866" alt="image" src="https://github.com/user-attachments/assets/0a98bb1c-69ef-4d22-8979-44824b65e00c" />

2) and then a simple logger to show it  in the ApiService project :

<img width="578" alt="image" src="https://github.com/user-attachments/assets/19bfbb37-47e2-474c-9bd5-94fa2489cfe6" />

When run locally it works great - two apiservice resources are started :

<img width="1217" alt="image" src="https://github.com/user-attachments/assets/82413415-18d3-4a10-8b5d-3dca15dde4c3" />

with arbitrary ports on each ("dashboard port" and "dashboard target port"):

<img width="492" alt="image" src="https://github.com/user-attachments/assets/34e1b677-0d37-4e6f-89e3-ad62c152d62c" />

However when attempting to deploy this scenario to Azure using "azd up" this error is generated :

<img width="1242" alt="image" src="https://github.com/user-attachments/assets/9baf1727-9fda-406d-a4ae-6313dfef2fe1" />


Since making the discussion post I have dug into this a little further and found that when running "azd up --debug" we can see this JSON being used to setup the container app :

```
2025/03/02 21:37:58 container_app.go:584: setting body to {"identity":{"type":"UserAssigned","userAssignedIdentities":{"/subscriptions/XXXXXXX/resourceGroups/rg-portapp/providers/Microsoft.ManagedIdentity/userAssignedIdentities/mi-7bzyyzka6tzec":{}}},"location":"uksouth","properties":{"configuration":{"activeRevisionsMode":"single","ingress":{"additionalPortMappings":[{"external":false,"targetPort":0}],"allowInsecure":false,"external":true,"targetPort":8000,"transport":"http"},"registries":[{"identity":"/subscriptions/XXX/resourceGroups/rg-portapp/providers/Microsoft.ManagedIdentity/userAssignedIdentities/mi-7bzyyzka6tzec","server":"acr7bzyyzka6tzec.azurecr.io"}],"runtime":{"dotnet":{"autoConfigureDataProtection":true}}},"environmentId":"/subscriptions/XXXX/resourceGroups/rg-portapp/providers/Microsoft.App/managedEnvironments/cae-7bzyyzka6tzec","template":{"containers":[{"env":[{"name":"AZURE_CLIENT_ID","value":"660c92b0-a7da-400a-93da-a212342698f0"},{"name":"APIDASHBOARDPORT","value":"8000"},{"name":"ASPNETCORE_FORWARDEDHEADERS_ENABLED","value":"true"},{"name":"HTTP_PORTS","value":"8080"},{"name":"OTEL_DOTNET_EXPERIMENTAL_OTLP_EMIT_EVENT_LOG_ATTRIBUTES","value":"true"},{"name":"OTEL_DOTNET_EXPERIMENTAL_OTLP_EMIT_EXCEPTION_LOG_ATTRIBUTES","value":"true"},{"name":"OTEL_DOTNET_EXPERIMENTAL_OTLP_RETRY","value":"in_memory"}],"image":"acr7bzyyzka6tzec.azurecr.io/aspire-sample/apiservice-portapp:azd-deploy-1740951475","name":"apiservice"}],"scale":{"minReplicas":1}}},"tags":{"aspire-resource-name":"apiservice","azd-service-name":"apiservice"}}
```

In particular I suspect this section is the problem :

```
	"location": "uksouth",
	"properties": {
		"configuration": {
			"activeRevisionsMode": "single",
			"ingress": {
				"additionalPortMappings": [
					{
						"external": false,
						"targetPort": 0
					}
				],
```

and I think this comes from the launchSettings.json having an applicationUrl in launch profile being used for that project (either the first one specifically selected).

If I remove the applicationUrl (or create a new profile without one and reference this in the AppHost) then the deployment works :

**AppHost.cs**
<img width="866" alt="image" src="https://github.com/user-attachments/assets/2b0ae0a8-1ba8-49af-8c45-530865a07db7" />

**launchSettings.json**  
<img width="415" alt="image" src="https://github.com/user-attachments/assets/0b20df69-78db-4049-882f-d4559ac44421" />

results in a deployment :

<img width="632" alt="image" src="https://github.com/user-attachments/assets/e76585e3-8492-4d1d-b61e-0b0daca6aba0" />

_Note that my specific problem is related to when integrating Orleans Silo projects with Aspire and the additional Orleans Dashboard which listens on an arbitrary port and provides insight into the Orleans deployment.  This project is just to illustrate the problem - there is no 'real' service running on the ports in this repro case._
  
_When running locally we have to ensure each Silo instance (replica) has a different port as of course we can't bind the same port > 1 time when running on localhost._
