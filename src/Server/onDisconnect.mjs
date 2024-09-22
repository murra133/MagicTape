import { DynamoDBClient, DeleteItemCommand, ScanCommand } from "@aws-sdk/client-dynamodb";
import { DeleteCommand, DynamoDBDocumentClient } from "@aws-sdk/lib-dynamodb";
import { ApiGatewayManagementApiClient, PostToConnectionCommand } from "@aws-sdk/client-apigatewaymanagementapi"; // ES Modules import





  export const handler = async (event, context) => {
    const client = new DynamoDBClient({'region':'us-west-1'});
    const ddb = DynamoDBDocumentClient.from(client);
    const command = new DeleteItemCommand({
        TableName: process.env.table,
        'Key': {
          'connection' : {'S' : event.requestContext.connectionId},
        },
      });
    const response = await client.send(command);
    
    
    const message = {}
    message['connection'] = event.requestContext.connectionId
    message['action'] = 'endConnection';
  const clientAPI = new ApiGatewayManagementApiClient({
        endpoint:
          'https://'+event.requestContext.domainName + '/' + event.requestContext.stage,
      })
      
  const command2 = new ScanCommand({
      TableName: process.env.table,
        "ExpressionAttributeNames": {
        "#CT": "connection",
      },
      "ExpressionAttributeValues": {
        ":dt": {
          "S": "client"
        }
      },
      "FilterExpression" : "client_type = :dt",
      "ProjectionExpression": "#CT",
    })
  const connectionIds = await client.send(command2)

  await Promise.all(connectionIds.Items.map(async ({ connection }) => {
    console.log(connection)
    if (connection.S !== event.requestContext.connectionId) {
      try {
        await clientAPI.send(new PostToConnectionCommand({
              'ConnectionId' : connection.S,
              'Data' : JSON.stringify(message)
          }))
      } catch (e) {
        console.log(e);
      }
    }
  }));  

    return response;
};


