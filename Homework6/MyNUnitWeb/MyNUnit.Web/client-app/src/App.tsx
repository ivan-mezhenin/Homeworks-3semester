import React, { useState } from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';
import FileUpload from './components/FileUpload';
import TestHistory from './components/TestHistory';
import TestRunner from './components/TestRunner';

function App() {
  const [activeTab, setActiveTab] = useState<'upload' | 'run' | 'history'>('upload');

  return (
      <div className="container mt-4">
        <h1 className="mb-4">MyNUnit Web Interface</h1>

        <ul className="nav nav-tabs mb-4">
          <li className="nav-item">
            <button
                className={`nav-link ${activeTab === 'upload' ? 'active' : ''}`}
                onClick={() => setActiveTab('upload')}
            >
              Upload Tests
            </button>
          </li>
          <li className="nav-item">
            <button
                className={`nav-link ${activeTab === 'run' ? 'active' : ''}`}
                onClick={() => setActiveTab('run')}
            >
              Run Tests
            </button>
          </li>
          <li className="nav-item">
            <button
                className={`nav-link ${activeTab === 'history' ? 'active' : ''}`}
                onClick={() => setActiveTab('history')}
            >
              Test History
            </button>
          </li>
        </ul>

        {activeTab === 'upload' && <FileUpload />}
        {activeTab === 'run' && <TestRunner />}
        {activeTab === 'history' && <TestHistory />}
      </div>
  );
}

export default App;