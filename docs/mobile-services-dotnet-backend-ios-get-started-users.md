<properties
	pageTitle="Add Authentication to Existing Azure Mobile Services App (iOS) | .NET Backend | Microsoft Azure"
	description="Learn how to use Mobile Services to authenticate users of your iOS app through a variety of identity providers, including Google, Facebook, Twitter, and Microsoft."
	services="mobile-services"
	documentationCenter="ios"
	authors="krisragh"
	manager="erikre"
	editor=""/>

<tags
	ms.service="mobile-services"
	ms.workload="mobile"
	ms.tgt_pltfrm="mobile-ios"
	ms.devlang="objective-c"
	ms.topic="article"
	ms.date="07/21/2016"
	ms.author="krisragh"/>

# Add Authentication to Existing Azure Mobile Services app

> [AZURE.SELECTOR-LIST (Platform | Backend )]
- [(iOS | .NET)](mobile-services-dotnet-backend-ios-get-started-users.md)
- [(iOS | JavaScript)](mobile-services-ios-get-started-users.md)
- [(Windows Runtime 8.1 universal C# | .NET)](mobile-services-dotnet-backend-windows-universal-dotnet-get-started-users.md)
- [(Windows Runtime 8.1 universal C# | Javascript)](mobile-services-javascript-backend-windows-universal-dotnet-get-started-users.md)
- [(Windows Phone Silverlight 8.x | Javascript)](mobile-services-windows-phone-get-started-users.md)
- [(Android | Javascript)](mobile-services-android-get-started-users.md)
- [(Xamarin.iOS | .NET)](mobile-services-dotnet-backend-xamarin-ios-get-started-users.md)
- [(Xamarin.iOS | Javascript)](partner-xamarin-mobile-services-ios-get-started-users.md)
- [(Xamarin.Android | .NET)](mobile-services-dotnet-backend-xamarin-android-get-started-users.md)
- [(Xamarin.Android | Javascript)](partner-xamarin-mobile-services-android-get-started-users.md)
- [(HTML | Javascript)](mobile-services-html-get-started-users.md)


&nbsp;

>[AZURE.WARNING] This is an **Azure Mobile Services** topic.  This service has been superseded by Azure App Service Mobile Apps and is scheduled for removal from Azure.  We recommend using Azure Mobile Apps for all new mobile backend deployments.  Read [this announcement](https://azure.microsoft.com/blog/transition-of-azure-mobile-services/) to learn more about the pending deprecation of this service.  
>
> Learn about [migrating your site to Azure App Service](https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-migrating-from-mobile-services/).
>
> Get started with Azure Mobile Apps, see the [Azure Mobile Apps documentation center](https://azure.microsoft.com/documentation/learning-paths/appservice-mobileapps/).

In this tutorial, you add authentication to the Quick Start project using a supported identity provider. This tutorial is based on the [Mobile Services Quick Start tutorial], which you must complete first.

##<a name="register"></a>Register app for authentication and configure Mobile Services


1. In the [Azure classic portal](https://manage.windowsazure.com/), click **Mobile Services** > your mobile service > **Dashboard**, and make a note of the **Mobile Service URL** value.

2. Register your app with one or more of the following authentication providers:
   * [Google](./
mobile-services-how-to-register-google-authentication.md)
   * [Facebook](./
mobile-services-how-to-register-facebook-authentication.md)
   * [Twitter](./
mobile-services-how-to-register-twitter-authentication.md)
   * [Microsoft](./
mobile-services-how-to-register-microsoft-authentication.md)
   * [Azure Active Directory](./
mobile-services-how-to-register-active-directory-authentication.md).

    Make a note of the client identity and client secret values generated by the provider. Do not distribute or share the client secret.

3. Back in the [Azure classic portal](https://manage.windowsazure.com/), click **Mobile Services** > your mobile service > **Identity** > your identity provider settings, then enter the client ID and secret value from your provider.

You've now configured both your app and your mobile service to work with your auth provider. You may optionally repeat all these steps for each additional identity provider you'd like to support.

> [AZURE.IMPORTANT] Verify that you've set the correct redirect URI on your identity provider's developer site. As described in the linked instructions for each provider above, the redirect URI may be different for a .NET backend service vs. for a JavaScript backend service. An incorrectly configured redirect URI may result in the login screen not being displayed properly and the app malfunctioning in unexpected ways.


### (Optional) Configure your .NET Mobile Service for Azure Active Directory

>[AZURE.NOTE] These steps are optional because they only apply to the Azure Active Directory login provider.

1. Install the [WindowsAzure.MobileServices.Backend.Security NuGet package](https://www.nuget.org/packages/WindowsAzure.MobileServices.Backend.Security).

2. In Visual Studio expand App_Start and open WebApiConfig.cs. Add the following `using` statement at the top:

        using Microsoft.WindowsAzure.Mobile.Service.Security.Providers;

3. Also in WebApiConfig.cs, add the following code to the `Register` method, immediately after `options` is instantiated:

        options.LoginProviders.Remove(typeof(AzureActiveDirectoryLoginProvider));
        options.LoginProviders.Add(typeof(AzureActiveDirectoryExtendedLoginProvider));

##<a name="permissions"></a>Restrict permissions to authenticated users



By default, all requests to mobile service resources are restricted to clients that present the application key, which does not strictly secure access to resources. To secure your resources, you must restrict access to only authenticated clients.

1. In Visual Studio, open your mobile service project, expand the Controllers folder, and open **TodoItemController.cs**. The **TodoItemController** class implements data access for the TodoItem table. Add the following `using` statement:

		using Microsoft.WindowsAzure.Mobile.Service.Security;

2. Apply the following _AuthorizeLevel_ attribute to the **TodoItemController** class.

		[AuthorizeLevel(AuthorizationLevel.User)]

	This makes sure that all operations against the _TodoItem_ table require an authenticated user. You can also apply the *AuthorizeLevel* attribute at the method level.

3. (Optional) If you wish to debug authentication locally, expand the `App_Start` folder, open **WebApiConfig.cs**, and add the following code to the **Register** method.  

		config.SetIsHosted(true);

	This tells the local mobile service project to run as if it is being hosted in Azure, including honoring the *AuthorizeLevel* settings. Without this setting, all HTTP requests to localhost are permitted without authentication despite the *AuthorizeLevel* setting. When you enable self-hosted mode, you also need to set a value for the local application key.

4. (Optional) In the web.config project file, set a string value for the `MS_ApplicationKey` app setting.

	This is the password that you use (with no username) to test the API help pages when you run the service locally.  This string value is not used by the live site in Azure, and you do not need to use the actual application key; any valid string value will work.

4. Republish your project.

In Xcode, open the project. Press the **Run** button to  start the app. Verify that an exception with a status code of 401 (Unauthorized) is raised after the app starts. This happens because the app attempts to access Mobile Services as an unauthenticated user, but the _TodoItem_ table now requires authentication.

##<a name="add-authentication"></a>Add authentication to app

* Open **QSTodoListViewController.m** and add the following method. Change _facebook_ to _microsoftaccount_, _twitter_, _google_, or _windowsazureactivedirectory_ if you're not using Facebook as your identity provider.

```
        - (void) loginAndGetData
        {
            MSClient *client = self.todoService.client;
            if (client.currentUser != nil) {
                return;
            }

            [client loginWithProvider:@"facebook" controller:self animated:YES completion:^(MSUser *user, NSError *error) {
                [self refresh];
            }];
        }
```

* Replace `[self refresh]` in `viewDidLoad` with the following:

```
        [self loginAndGetData];
```

* Press  **Run** to start the app, and then log in. When you are logged in, you should be able to view the Todo list and make updates.

##<a name="store-authentication"></a>Store authentication tokens in app


The previous example contacts both the identity provider and the mobile service every time the app starts. Instead, you can cache the authorization token and try to use it first.

* The recommended way to encrypt and store authentication tokens on an iOS client is use the iOS Keychain. We'll use [SSKeychain](https://github.com/soffes/sskeychain) -- a simple wrapper around the iOS Keychain. Follow the instructions on the SSKeychain page and add it to your project. Verify that the **Enable Modules** setting is enabled in the project's **Build Settings** (section **Apple LLVM - Languages - Modules**.)

* Open **QSTodoListViewController.m** and add the following code:

```
		- (void) saveAuthInfo {
				[SSKeychain setPassword:self.todoService.client.currentUser.mobileServiceAuthenticationToken forService:@"AzureMobileServiceTutorial" account:self.todoService.client.currentUser.userId]
		}


		- (void)loadAuthInfo {
				NSString *userid = [[SSKeychain accountsForService:@"AzureMobileServiceTutorial"][0] valueForKey:@"acct"];
		    if (userid) {
		        NSLog(@"userid: %@", userid);
		        self.todoService.client.currentUser = [[MSUser alloc] initWithUserId:userid];
		         self.todoService.client.currentUser.mobileServiceAuthenticationToken = [SSKeychain passwordForService:@"AzureMobileServiceTutorial" account:userid];

		    }
		}
```

* In `loginAndGetData`, modify  `loginWithProvider:controller:animated:completion:`'s completion block. Add the following line right before `[self refresh]` to store the user ID and token properties:

```
				[self saveAuthInfo];
```

* Let's load the user ID and token when the app starts. In the `viewDidLoad` in **QSTodoListViewController.m**, add this right after`self.todoService` is initialized.

```
				[self loadAuthInfo];
```

##<a name="next-steps"></a>Next steps

In the next tutorial, [Service-side authorization of Mobile Services users], you will user the user ID value to filter returned data.

<!-- Anchors. -->
[Register your app for authentication and configure Mobile Services]: #register
[Restrict table permissions to authenticated users]: #permissions
[Add authentication to the app]: #add-authentication
[Next Steps]:#next-steps
[Storing authentication tokens in your app]:#store-authentication

<!-- URLs. -->
[Service-side authorization of Mobile Services users]: mobile-services-dotnet-backend-service-side-authorization.md
[Mobile Services Quick Start tutorial]: mobile-services-dotnet-backend-ios-get-started.md
[Get started with authentication]: mobile-services-dotnet-backend-ios-get-started-users.md
[Get started with push notifications]: mobile-services-dotnet-backend-ios-get-started-push.md
[Authorize users with scripts]: mobile-services-dotnet-backend-service-side-authorization.md
[Mobile Services .NET How-to Conceptual Reference]: https://azure.microsoft.com/develop/mobile/how-to-guides/work-with-net-client-library
[Register your Windows Store app package for Microsoft authentication]: mobile-services-how-to-register-store-app-package-microsoft-authentication.md
