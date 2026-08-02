pipeline {
    agent any
 
 
    stages {
        

        stage ('Build Debug') 
        {
            steps 
            {
                dir('src/Tool')
                {
                    sh 'echo "$PWD"'
                    dotnetBuild project: 'BurnSystems.Make.Tool.slnx', workDirectory: '.'
                }
            }
        }

        stage ('Build Release')
        {
            steps
            {
                dotnetBuild configuration: 'Release', project: 'src/Tool/BurnSystems.Make.Tool.slnx', workDirectory: './src/Tool/'
            }
        }    

        stage ('Test Debug')
        {
            steps
            {
                dotnetTest logger: 'trx;LogFileName=test.trx', project: 'src/Tool/BurnSystems.Make.Test/BurnSystems.Make.Test.csproj', continueOnError: true, noBuild: true
                mstest()
            }
        }

        stage ('Test Release')
        {
            steps
            {
                dotnetTest logger: 'trx;LogFileName=test.trx', project: 'src/Tool/BurnSystems.Make.Test/BurnSystems.Make.Test.csproj', configuration: 'Release', continueOnError: true, noBuild: true
                mstest()
            }
        }
    }   
}