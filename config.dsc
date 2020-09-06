config({
    resolvers: [
        {
            kind: "MsBuild",
            moduleName: "Compilers",
            root: d`.`,
            msBuildSearchLocations: [d`C:\Program Files (x86)\Microsoft Visual Studio\2019\Preview\MSBuild\Current\Bin`],
            fileNameEntryPoints: [r`Compilers.sln`]
        }
    ]
});