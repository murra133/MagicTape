import { DynamoDBClient, PutItemCommand } from "@aws-sdk/client-dynamodb";
import { PutCommand, DynamoDBDocumentClient } from "@aws-sdk/lib-dynamodb";

  export const handler = async (event, context) => {
    const client = new DynamoDBClient({'region':'us-west-1'});
    const ddb = DynamoDBDocumentClient.from(client);
    const command = new PutItemCommand({
        TableName: process.env.table,
        Item: {
          'connection' : {S :event.requestContext.connectionId},
          'action' : {S:'Hello'}
        },
      });
    const response = await client.send(command);

      return {
        statusCode: 200,
      };
};

