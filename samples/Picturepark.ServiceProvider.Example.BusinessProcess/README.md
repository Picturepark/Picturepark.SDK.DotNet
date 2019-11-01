# Business process service provider sample
This sample shows the usage of a service provider to react on the execution of a business rule in the CP backend,
creating a notification about a then triggered long-running task for the user. The user is able to cancel that task from the CP UI.

## Description
The service provider waits for a `BusinessRuleFired` event for a specific rule. The following steps are then performed:

1. Gather created content ids into batches
2. After an inactivity timeout elapsed or a batch is complete, a business process is created
3. For each content, the original is downloaded into a local folder. After every content, the notification shown to the user in the CP UI is updated to reflect the progress
4. As long as the business process is not cancelled, this is repeated for each content
5. On cancellation by the user, the process is stopped
6. At the end, the business process is marked as finished (or cancelled)

## Configuration
### Create service provider
To create the service provider, run the following call against the CP management API:

```
POST {{cloudmanager-url}}/service/serviceProvider:
{
    "externalId": "bpedemo",
    "name": "Business Process Engine Provider (POC)",
    "secret": "CHANGEME"
}
```

*Note: The `externalId` must match the `serviceProviderId` setting in the application configuration.*

To register the service provider for a customer, run the following call against the CP management API:

```
POST {{cloudmanager-url}}/service/customer/serviceProvider:
{
    "customerAlias" : "{{customer-alias}}",
    "serviceProvider": {
        "id": "bpedemo",
        "scopes": [
                    "$.[?(@.applicationEvent.kind == 'BusinessRuleFiredEvent' && @.applicationEvent.details..*.ruleIds[*] == 'ImageUploaded')]",
                    "$.[?(@.applicationEvent.kind == 'BusinessProcessCancellationRequestedEvent')]"
                  ],
       "allowedMessages": [],
	   "allowedCommands": []
    }
}
```

*Note: The `ruleId` in the `scopes` property of the registration request must match the `triggeringBusinessRuleId` setting in the application configuration.*

*Note: The `externalId` in the registration request must match the `externalId` in the creation request above.*

### Create the business rule
Create a business rule either using the API or the CP UI with the `id` matching the `ruleId` specified when registering the service provider for the customer.

Use the example rule that triggers whenever a new image has been uploaded as a starting point:

```
{
    "kind": "BusinessRuleConfigurable",
    "condition": {
        "kind": "ContentSchemaCondition",
        "schemaId": "ImageMetadata"
    },
    "actions": [
        {
            "kind": "ProduceMessageAction"
        }
    ],
    "id": "ImageUploaded",
    "triggerPoint": {
        "executionScope": "MainDoc",
        "documentType": "Content",
        "action": "Create"
    },
    "isEnabled": true,
    "names": {
        "en": "Image uploaded trigger"
    },
    "description": {
        "en": "Triggers when a new content is created having ContentSchemaId = 'ImageMetadata'"
    }
}
```

### Application settings

Copy the `appsettings_template.json` file to `appsettings.json` and adjust it to suite your environment.

Below follows a description of all settings in the `config` section of `appsettings.json`:

* `apiUrl`: URL to the API of your CP instance
* `customerAlias`: the customer alias of your customer
* `accessToken`: your access token for the CP API
* `serviceProviderId`: ID your created the service provider with
* `integrationHost`: Hostname/address of the integration bus host
* `integrationPort`: Port of the integration bus
* `useSsl`: set to `true` to use an encrypted connection to the integration bus
* `secret`: the secret you provided when creating the service provider
* `triggeringBusinessRuleId`: ID of the business rule that should trigger the service provider
* `batchSize`: Maximum number of contents to download in each batch
* `inactivityTimeout`: Timeout after which a batch is downloaded regardless of the size
* `outputDownloadDirectory`: Path where the downloaded outputs should be stored

## Running

1. Run the application
2. Upload images to CP
3. The service provider will download the images, creating a notification in the UI

## Structure
Namespaces:
* `Config`: Contains the configuration for the sample, see below for instructions
* `MessageHandler`: Contains handlers for the incoming messages on the livestream
* `BusinessProcess`: Contains infrastructure to work with business processes
* `Util`: Utilities

Important classes:
* `LiveStreamSubscriber`: Subscribes to the livestream, handles incoming messages by calling an `IApplicationEventHandler`
* `ContentBatchOperationPerformer`: Gathers created contents into batches and then downloads each batch into a local folder.