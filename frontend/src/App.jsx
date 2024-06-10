import React, { useState } from 'react';
import Signup from './Signup';
import Login from './Login';
import UserList from './UserList';

const App = () => {
  const [token, setToken] = useState('');

  return (
    <div>
      <h1>User Authentication</h1>
      {!token ? (
        <>
          <Signup />
          <Login setToken={setToken} />
        </>
      ) : (
        <UserList token={token} />
      )}
    </div>
  );
};

export default App;
