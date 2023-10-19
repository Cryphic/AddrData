import React from 'react';

const ExportData = ({ data }) => {
    const download = () => {
        const element = document.createElement("a");
        const file = new Blob([JSON.stringify(data, null, 2)], {
        type: "text/plain;charset=utf-8"
        });
        element.href = URL.createObjectURL(file);
        element.download = "data.json";
        document.body.appendChild(element);
        element.click();
    };
    
    return (
        <div className="mt-8">
        <button
            className="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
            onClick={download}
        >
            Download .json
        </button>
        </div>
    );
    };

export default ExportData;
