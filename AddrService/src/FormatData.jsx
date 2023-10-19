import React from 'react';

const Key = ({ text }) => <span className="text-blue-200">{text}:</span>;

const Subkey = ({ text }) => <span className="text-blue-500">{text}</span>;

const Value = ({ value }) => {
  const formattedValue = JSON.stringify(value, null, 2)
    .split("\n")
    .map((item, index) => (
      <React.Fragment key={index}>
        {item.split(":").map((subitem, subindex) => (
          <React.Fragment key={subindex}>
            {subindex === 0 ? (
              <Subkey text={subitem} />
            ) : (
              `:${subitem}`
            )}
          </React.Fragment>
        ))}
        <br />
      </React.Fragment>
    ));

  return <span className="text-gray-500">{formattedValue}</span>;
};

const SourceData = ({ source, data }) => (
  <div className="mb-4">
    <h3 className="text-xl font-semibold mb-2 text-blue-500">{source}:</h3>
    <ul className="list-inside list-disc">
      {Object.keys(data).map((key, keyIndex) => (
        <li key={keyIndex} className="text-white">
          <Key text={key} />
          {' '}
          <Value value={data[key]} />
        </li>
      ))}
    </ul>
  </div>
);

const formatData = (data) => (
  <div>
    {Object.keys(data).map((source, sourceIndex) => (
      <SourceData key={sourceIndex} source={source} data={data[source]} />
    ))}
  </div>
);

export default formatData;
