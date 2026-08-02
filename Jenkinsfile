pipeline {
    agent any
 
 
    stages {
        

        stage ('Build Debug') 
        {
            steps 
            {
                // Shell build step
                sh 'cd src/Tool'
                sh 'echo "$PWD"'
                dotnetBuild project: 'BurnSystems.Make.Tool.slnx', workDirectory: '.'
                sh 'cd ../..'
            }
        }

        stage ('Build Release')
        {
            steps
            {
                sh 'cd src/Tool'
                dotnetBuild configuration: 'Release', project: 'BurnSystems.Make.Tool.slnx', workDirectory: '.'
                sh 'cd ../..'
            }
        }    

        stage ('Test Debug')
        {
            steps
            {
                sh 'cd src/Tool'
                dotnetTest logger: 'trx;LogFileName=test.trx', project: 'BurnSystems.Make.Test/BurnSystems.Make.Test.csproj', continueOnError: true, noBuild: true
                mstest()
                sh 'cd ../..'
            }
        }

        stage ('Test Release')
        {
            steps
            {
                sh 'cd src/Tool'
                dotnetTest logger: 'trx;LogFileName=test.trx', project: 'BurnSystems.Make.Test/BurnSystems.Make.Test.csproj', configuration: 'Release', continueOnError: true, noBuild: true
                mstest()
                sh 'cd ../..'
            }
        }
    }   
}