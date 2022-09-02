# Duende/IdentityServer Playground

This is a prototype authentication server based on the Duende IdentityServer, and samples largely from their [Quickstart guides](https://docs.duendesoftware.com/identityserver/v5/quickstarts/) and [nuget templates](https://www.nuget.org/packages/Duende.IdentityServer.Templates/5.1.0)

There are three main projects to this prototype
- **Api**: A dummy, protected API that requires authentication for server-to-server communication. By default, it runs locally on port 6001.
- **IdentityServer**: The identity server. By default, it runs locally on port 5001, and the discovery document can be found here https://localhost:5001/.well-known/openid-configuration
- **MVCClient**: A MVC user interface ontop of the Identity Server web application. By default, the web app is self-hosted on port 5002.

You can play around with authentication using the test users available. For account information, see `DuendeIdentityServer/src/IdentityServer/Quickstart/TestUsers.cs`

# Installation

To build from source:

```
git clone https://github.com/AlvinLee-AI/Playground.git
cd DuendeIdentityServer
dotnet build
```
</br>

To run the IdentityServer:
```
cd DuendeIdentityServer/src/IdentityServer
dotnet build
dotnet run
```

To add Google authentication to IdentityServer, make sure a Google OAuth Client project is set up, and the environment variables `OIDC_GOOGLE_CLIENTID` and `OIDC_GOOGLE_SECRET` set to the project's `Client ID` and `Client Secret`, respectively, are available.

</br>

To run the UI:
```
cd DuendeIdentityServer/src/MVCClient
dotnet build
dotnet run
```

