call reportgenerator -reports:**/coverage.cobertura.xml -targetdir:Coverage "-reporttypes:HtmlInline_AzurePipelines;cobertura"

start .\Coverage\index.htm
