import React, { useEffect, useState } from 'react';

function App() {
  const [playlists, setPlaylists] = useState([]);
  const [selected, setSelected] = useState(null);
  const [tracks, setTracks] = useState([]);
  const [newPlaylist, setNewPlaylist] = useState('');
  const [newTrack, setNewTrack] = useState({ title: '', artist: '' });

  useEffect(() => {
    fetch('/playlists')
      .then(res => res.json())
      .then(setPlaylists);
  }, []);

  const fetchTracks = (id) => {
    fetch(`/playlists/${id}`)
      .then(res => res.json())
      .then(data => {
        setSelected(id);
        setTracks(data.tracks || []);
      });
  };

  const handleCreatePlaylist = (e) => {
    e.preventDefault();
    fetch('/playlists', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name: newPlaylist })
    })
      .then(res => res.json())
      .then(pl => {
        setPlaylists([...playlists, pl]);
        setNewPlaylist('');
      });
  };

  const handleAddTrack = (e) => {
    e.preventDefault();
    fetch(`/playlists/${selected}/tracks`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        id: Math.random().toString(36).slice(2),
        title: newTrack.title,
        artist: newTrack.artist
      })
    })
      .then(res => res.json())
      .then(() => {
        fetchTracks(selected);
        setNewTrack({ title: '', artist: '' });
      });
  };

  return (
    <div style={{ maxWidth: 700, margin: '2rem auto', fontFamily: 'sans-serif' }}>
      <h1>🎶 Music Playlists</h1>
      <form onSubmit={handleCreatePlaylist} style={{ marginBottom: 20 }}>
        <input
          value={newPlaylist}
          onChange={e => setNewPlaylist(e.target.value)}
          placeholder="New playlist name"
          required
        />
        <button type="submit">Create Playlist</button>
      </form>
      <div style={{ display: 'flex', gap: 40 }}>
        <div style={{ flex: 1 }}>
          <h2>Playlists</h2>
          <ul>
            {playlists.map(pl => (
              <li key={pl.id}>
                <button onClick={() => fetchTracks(pl.id)} style={{ fontWeight: pl.id === selected ? 'bold' : 'normal' }}>
                  {pl.name}
                </button>
              </li>
            ))}
          </ul>
        </div>
        <div style={{ flex: 2 }}>
          {selected && (
            <>
              <h2>Tracks</h2>
              <form onSubmit={handleAddTrack} style={{ marginBottom: 10 }}>
                <input
                  value={newTrack.title}
                  onChange={e => setNewTrack({ ...newTrack, title: e.target.value })}
                  placeholder="Track title"
                  required
                />
                <input
                  value={newTrack.artist}
                  onChange={e => setNewTrack({ ...newTrack, artist: e.target.value })}
                  placeholder="Artist"
                  required
                />
                <button type="submit">Add Track</button>
              </form>
              <ul>
                {tracks.map(t => (
                  <li key={t.id}>
                    <b>{t.title}</b> <i>by {t.artist}</i>
                  </li>
                ))}
              </ul>
            </>
          )}
        </div>
      </div>
    </div>
  );
}

export default App;
