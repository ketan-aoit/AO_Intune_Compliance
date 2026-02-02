import { Routes, Route, Navigate } from 'react-router-dom';
import { Layout } from '../components/Layout';
import { Dashboard } from '../pages/Dashboard';
import { Devices } from '../pages/Devices';
import { DeviceDetail } from '../pages/DeviceDetail';
import { Alerts } from '../pages/Alerts';
import { ComplianceRules } from '../pages/ComplianceRules';
import { Settings } from '../pages/Settings';

export function AppRoutes() {
  return (
    <Layout>
      <Routes>
        <Route path="/" element={<Dashboard />} />
        <Route path="/devices" element={<Devices />} />
        <Route path="/devices/:id" element={<DeviceDetail />} />
        <Route path="/alerts" element={<Alerts />} />
        <Route path="/compliance-rules" element={<ComplianceRules />} />
        <Route path="/settings" element={<Settings />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Layout>
  );
}
