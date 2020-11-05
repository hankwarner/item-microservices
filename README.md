# Description

Azure HTTP function app that provides data related to Ferguson items, such as descriptions, weights, preferred shipping methods, stocking statuses, vendors, etc.


# OpenAPI Definition

https://item-microservices.azurewebsites.net/api/swagger/ui?code=tt4jb8M85xwRpGDJRn/4BRiDxsJQUEM2tMgu2Gx78Q0WJCyfygaKwg==


# CI/CD workflow and Pull Request instructions

1. Create a new branch with a name suitable for the code being added/refactored.

2. Send initial pull request to the **test** branch. Once merged, a build & deploy action in **Debug** configuration will be triggered to the _item-microservices-test_ function app. OpenAPI defintion here: https://item-microservices-test.azurewebsites.net/api/swagger/ui?code=YNRDXSumlF7ePREo4g45Aqw89yPoBItF/1jREI7PIzO0FPEwAsHznQ==

3. Once approved in test, the next pull request should be sent to the **staging** branch. Once merged, a build & deploy action in **Release** configuration will be triggered to the _item-microservices_ staging environment (a deployment slot in the production function app). OpenAPI defintion here: https://item-microservices-staging.azurewebsites.net/api/swagger/ui?code=l4QvA75lbnDdkGpJ7F4KTzmXDRSLYWfbiKOgDI87jPTw4t72pVce6A==

4. Once approved in staging, the final pull request should be sent to the **master** branch. Once merged, a build & deploy action in **Release** configuration will be triggered to the _item-microservices_ production environment. OpenAPI defintion in section above.
