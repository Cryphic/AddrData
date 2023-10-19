import { useState, useEffect } from 'react';

function StatusStrip({ apiUrl }) {
  const [isOnline, setIsOnline] = useState(false);

  useEffect(() => {
    const checkConnection = async () => {
      try {
        const response = await fetch(apiUrl);
        if (response.ok) {
          setIsOnline(true);
        } else {
          setIsOnline(false);
        }
      } catch (error) {
        setIsOnline(false);
      }
    };

    checkConnection();
    const intervalId = setInterval(checkConnection, 5000);

    return () => clearInterval(intervalId);
  }, [apiUrl]);

  return (
    <div className="fixed top-0 left-0 w-full bg-gray-800 text-white">
      <div className="flex items-center justify-between px-4 py-2">
        <div className="flex items-center">
          <div className={`rounded-full w-4 h-4 mr-2 ${isOnline ? 'bg-green-500' : 'bg-red-500'}`}></div>
          <span>API - {isOnline ? 'Online' : 'Offline'}</span>
        </div>
        <div>
        </div>
      </div>
    </div>
  );
}

export default StatusStrip;