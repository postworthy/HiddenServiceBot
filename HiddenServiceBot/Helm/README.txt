#This is an example of how to deploy the chart and pass in override values
helm install --set TELEGRAM_API_KEY=ABC --set QUICK_START_URL=XYZ hidden-service hidden-service-bot-1.0.2.tgz --namespace MY-NAMESPACE

#This is an example of how to run the docker image directly
docker run -it -e TELEGRAM_API_KEY=ABC -e QUICK_START_URL=XYZ kvrg/hiddenservicebot:1.0.2