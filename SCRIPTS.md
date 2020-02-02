# Build scripts

First, install the node modules:

    npm install

Then, run one of the following scripts:

    npm run nswag

Regenerates all clients based on the Swagger specifications located in the "/swagger" directory:

    npm run build

Compiles all projects and creates NuGet packages in the directory "/build/Packages":

    npm run tests

Runs the unit tests in the "Picturepark.SDK.V1.Tests" project
