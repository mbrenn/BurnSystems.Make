pipeline {
    agent any
 
 
    stages {
        

        stage ('Build Debug') 
        {
            steps 
            {
                // Shell build step
                dotnetBuild project: 'src/Tool/BurnSystems Make Tool.slnx', workDirectory: './src/Tool'
            }
        }

        stage ('Build Release')
        {
            steps
            {
                dotnetBuild configuration: 'Release', project: 'src/Tool/BurnSystems Make Tool.slnx', workDirectory: './src/Tool'
            }
        }    

        stage ('Test Debug')
        {
            steps
            {
                dotnetTest logger: 'trx;LogFileName=test.trx', project: 'src/Tools/BurnSystems.Make.Test/BurnSystems.Make.Test.csproj', continueOnError: true, noBuild: true
                mstest()
            }
        }

        stage ('Test Release')
        {
            steps
            {
                dotnetTest logger: 'trx;LogFileName=test.trx', project: 'src/Tools/BurnSystems.Make.Test/BurnSystems.Make.Test.csproj', configuration: 'Release', continueOnError: true, noBuild: true
                mstest()
            }
        }
    }   
}