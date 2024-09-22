import { DynamoDBClient, ScanCommand } from "@aws-sdk/client-dynamodb"; // ES Modules import
import { ApiGatewayManagementApiClient, PostToConnectionCommand } from "@aws-sdk/client-apigatewaymanagementapi"; // ES Modules import

export const handler = async (event,context) => {
  console.log(event)
  console.log(context)
  const client = new DynamoDBClient({'region':'us-west-1'});
  const command = new ScanCommand({
      TableName: process.env.table,
    })
  const connections = await client.send(command)
  console.log(connections)
  let message = JSON.parse(event.body);
  const clientAPI = new ApiGatewayManagementApiClient({
          endpoint:
            'https://'+event.requestContext.domainName + '/' + event.requestContext.stage,
        })
    await Promise.all(connections.Items.map(async ({ connection }) => {
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

  const response = {
    statusCode: 200,
  }
    
  return response;
};