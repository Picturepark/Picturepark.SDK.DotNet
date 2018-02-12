# Sample: Picturepark.ContentUploader
## Hybrid OIDC authentication in WPF

The Picturepark.ContentUploader sample project shows how to authenticate with the Picturepark API and upload an image.

## Setup

In order to use the sample app, you need to setup an OIDC Hybrid client in the Picturepark backend settings: 

- Login to your Picturepark backend, navigate to `Settings` > `API Clients` and create a new client
- Define a Client ID, Client Name and a client secret and the following fields:
  - "Authorization Flow": `Hybrid`
  - "Allowed Scopes": `all_scopes`
  - "Redirect Uris" and "Allowed CORS origins": `http://localhost/wpf` (this can be any URL as long they are the same in `Picturepark.ContentUploader` and in the created client)
  - Enable "Allow offline access"
  - Enable the client ("Enabled")
- After creating the client, start the Picturepark.ContentUploader and fill in the same values as specified before.
- Select an image and upload it to the Picturepark backend (it should apprear under "Transfers" in the Picturepark backend)