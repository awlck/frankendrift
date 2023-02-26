import {dotnet} from './dotnet.js'

const {setModuleImports, getAssemblyExports, getConfig} = await dotnet.create()

setModuleImports('main.js', {
})

const config = getConfig()
const exports = await getAssemblyExports(config.mainAssemblyName)

await dotnet.run()