import React, { useEffect, useState } from 'react';
import axios from 'axios';

const UserList = ({ token }) => {
  const [users, setUsers] = useState([]);
  const [editingUser, setEditingUser] = useState(null);
  const [updatedEmail, setUpdatedEmail] = useState('');
  const [updatedPassword, setUpdatedPassword] = useState('');

  useEffect(() => {
    const fetchUsers = async () => {
      try {
        const response = await axios.get('http://localhost:5127/users', {
          headers: { Authorization: `Bearer ${token}` },
        });
        setUsers(response.data);
      } catch (error) {
        console.error(error);
      }
    };

    fetchUsers();
  }, [token]);

  const handleDelete = async (id) => {
    try {
      await axios.delete(`http://localhost:5127/users/${id}`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      setUsers(users.filter(user => user.id !== id));
    } catch (error) {
      console.error(error);
    }
  };

  const handleUpdate = async (e) => {
    e.preventDefault();
    try {
      await axios.put(`http://localhost:5127/users/${editingUser.id}`, {
        email: updatedEmail,
        password: updatedPassword,
      }, {
        headers: { Authorization: `Bearer ${token}` },
      });
      setUsers(users.map(user => 
        user.id === editingUser.id 
          ? { ...user, email: updatedEmail, password: updatedPassword } 
          : user
      ));
      setEditingUser(null);
      setUpdatedEmail('');
      setUpdatedPassword('');
    } catch (error) {
      console.error(error);
    }
  };

  const startEditing = (user) => {
    setEditingUser(user);
    setUpdatedEmail(user.email);
    setUpdatedPassword('');
  };

  return (
    <div>
      <h2>User List</h2>
      <ul>
        {users.map((user) => (
          <li key={user.id}>
            {user.email}
            <button onClick={() => startEditing(user)}>Edit</button>
            <button onClick={() => handleDelete(user.id)}>Delete</button>
          </li>
        ))}
      </ul>
      {editingUser && (
        <form onSubmit={handleUpdate}>
          <h2>Update User</h2>
          <input
            type="email"
            placeholder="New Email"
            value={updatedEmail}
            onChange={(e) => setUpdatedEmail(e.target.value)}
            required
          />
          <input
            type="password"
            placeholder="New Password"
            value={updatedPassword}
            onChange={(e) => setUpdatedPassword(e.target.value)}
            required
          />
          <button type="submit">Update</button>
          <button type="button" onClick={() => setEditingUser(null)}>Cancel</button>
        </form>
      )}
    </div>
  );
};

export default UserList;
