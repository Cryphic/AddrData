import React, { useState, useEffect } from 'react';
import axios from 'axios';
import formatData from './FormatData';
import ExportData from './ExportData';
import StatusStrip from './StatusStrip';
import ExecuteServerFunction from './ExecuteServerFunction';

function AddrSearch() {
  const [ip, setIp] = useState('');
  const [data, setData] = useState(null);
  const [error, setError] = useState(null);
  const [clients, setClients] = useState([]);
  const [selectedClient, setSelectedClient] = useState('');

  useEffect(() => {
    const fetchClients = async () => {
      try {
        const response = await axios.get('http://localhost:5000/api/query/clients');
        setClients(response.data);
      } catch (error) {
        console.error(error);
      }
    };
    fetchClients();
  }, []);

  const handleSearch = async () => {
    try {
      const response = await axios.get(`http://localhost:5000/api/info/${ip}`);
      setData(response.data);
      setError(null);
    } catch (error) {
      setError('An error occurred while fetching data. Please try again later.');
      setData(null);
    }
  };

  const handleClientChange = (e) => {
    setSelectedClient(e.target.value);
  };

  return (
    <div>
      <StatusStrip apiUrl="http://localhost:5000/" />
      <div className="min-h-screen flex flex-col items-center justify-center bg-gray-900 text-white mt-8">
        <div className="w-full max-w-md p-4 flex flex-col items-center">
          <h1 className="text-2xl text-center">IP Address Info Lookup</h1>
          <small className="mb-4">AddrDataWeb API [VirusTotal, AbuseIPDB]</small>
          <div className="flex items-center space-x-2 mb-4">
            <input
              type="text"
              placeholder="Enter an IP address"
              value={ip}
              onChange={(e) => setIp(e.target.value)}
              className="flex-1 py-2 px-3 bg-gray-800 text-white rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            <button onClick={handleSearch} className="bg-blue-500 hover:bg-blue-600 text-white py-2 px-4 rounded-md">
              Search
            </button>
          </div>
          {error && <p className="text-red-500">{error}</p>}
          {data && (
            <div className="rounded-md overflow-hidden border border-gray-800 p-4 flex-1">
              <h2 className="text-lg font-semibold mb-2">IP Address Information</h2>
              {formatData(data)}
              <div className="flex items-center space-x-2 mb-4">
                <label htmlFor="clients" className="text-white">Select a client:</label>
                <select id="clients" value={selectedClient} onChange={handleClientChange} className="bg-gray-800 text-white rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 w-full">
                  <option value="">Select a client</option>
                  {clients.map((client) => (
                    <option key={client} value={client}>{client}</option>
                  ))}
                </select>
                <ExecuteServerFunction client={selectedClient} ip={ip} />
              </div>
              <small>The current searched IP will be sent to the client for blocking.</small>
              <ExportData data={data} />
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default AddrSearch;
