import { Navigate, Route, Routes } from 'react-router-dom';
import { AppLayout } from './layout/AppLayout';
import { ErrorMonitorPage } from './pages/ErrorMonitor/ErrorMonitorPage';

function App() {
  return (
    <AppLayout>
      <Routes>
        <Route path="/" element={<Navigate to="/errors" replace />} />
        <Route path="/errors" element={<ErrorMonitorPage />} />
        <Route path="*" element={<Navigate to="/errors" replace />} />
      </Routes>
    </AppLayout>
  );
}

export default App;
