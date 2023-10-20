import React, { useEffect, useState } from 'react';
import * as signalR from "@microsoft/signalr";

const ExecuteServerFunction = ({ client, ip }) => {
  const [connection, setConnection] = useState(null);
  const [response, setResponse] = useState(null);

  useEffect(() => {
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5000/blockhub")
      .build();

    newConnection
      .start()
      .then(() => {
        console.log("Connected to SignalR Hub");
      })
      .catch((error) => {
        console.error("SignalR Connection Error: ", error);
      });

    newConnection.on("BlockIPResponse", (message) => {
      setResponse(message);
    });

    setConnection(newConnection);
  }, []);

  const handleExecuteBlockIP = async () => {
    if (connection) {
      try {
        await connection.invoke("ExecuteBlockIP", client, ip);
      } catch (error) {
        console.error("Error invoking ExecuteBlockIP: ", error);
      }
    }
  };

  return (
    <div>
      <button
        className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-2 rounded"
        onClick={handleExecuteBlockIP}
      >
        Block
      </button>
      {response && (
        <div className="mt-4 text-gray-600">{response}</div>
      )}
    </div>
  );
};

export default ExecuteServerFunction;
