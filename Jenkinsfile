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
                dir('src/Tool')
                {
                    dotnetBuild configuration: 'Release', project: 'BurnSystems.Make.Tool.slnx', workDirectory: '.'
                }
            }
        }    

        stage ('Test Debug')
        {
            steps
            {
                dir('src/Tool')
                {
                    dotnetTest logger: 'trx;LogFileName=test.trx', project: 'BurnSystems.Make.Test/BurnSystems.Make.Test.csproj', continueOnError: true, noBuild: true
                    mstest()
                }
            }
        }

        stage ('Test Release')
        {
            steps
            {
                dir('src/Tool')
                {
                    dotnetTest logger: 'trx;LogFileName=test.trx', project: 'BurnSystems.Make.Test/BurnSystems.Make.Test.csproj', configuration: 'Release', continueOnError: true, noBuild: true
                    mstest()
                }
            }
        }
    }   
}