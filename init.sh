#!/bin/sh
set -e

#	Run Docker and create localstack
#	docker start localstack
docker rm -f localstack
docker run -d --name localstack -p 4566:4566 localstack/localstack

SET=AWS_ACCESS_KEY_ID=mock
SET=AWS_SECRET_ACCESS_KEY=mock
SET=AWS_DEFAULT_REGION=us-east-1
SET=AWS_ENDPOINT_URL=http://localhost:4566

# Create DynamoDB table for file's metadata
awslocal --endpoint-url=http://localhost:4566 --region us-east-1 dynamodb create-table \
    --table-name Files \
    --attribute-definitions AttributeName=Filename,AttributeType=S AttributeName=UploadedAt,AttributeType=S \
    --key-schema AttributeName=Filename,KeyType=HASH AttributeName=UploadedAt,KeyType=RANGE \
    --provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5

# Create S3 bucket
awslocal --endpoint-url=http://localhost:4566 --region us-east-1 s3 mb s3://storage