import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from './client';
import type {
  PaginatedList,
  DeviceDto,
  DeviceDetailDto,
  ComplianceSummaryDto,
  DashboardDto,
  ComplianceRuleDto,
  AlertRecipientDto,
  AlertHistoryDto,
  SyncDevicesResult,
} from '../types/api';

// Dashboard
export function useDashboard() {
  return useQuery({
    queryKey: ['dashboard'],
    queryFn: async () => {
      const response = await apiClient.get<DashboardDto>('/dashboard');
      return response.data;
    },
  });
}

// Devices
export interface GetDevicesParams {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  complianceState?: string;
  osType?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

export function useDevices(params: GetDevicesParams = {}) {
  return useQuery({
    queryKey: ['devices', params],
    queryFn: async () => {
      const response = await apiClient.get<PaginatedList<DeviceDto>>('/devices', { params });
      return response.data;
    },
  });
}

export function useDevice(id: string) {
  return useQuery({
    queryKey: ['device', id],
    queryFn: async () => {
      const response = await apiClient.get<DeviceDetailDto>(`/devices/${id}`);
      return response.data;
    },
    enabled: !!id,
  });
}

export function useComplianceSummary() {
  return useQuery({
    queryKey: ['compliance-summary'],
    queryFn: async () => {
      const response = await apiClient.get<ComplianceSummaryDto>('/devices/compliance-summary');
      return response.data;
    },
  });
}

export function useSyncDevices() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async () => {
      const response = await apiClient.post<SyncDevicesResult>('/devices/sync');
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['devices'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
      queryClient.invalidateQueries({ queryKey: ['compliance-summary'] });
    },
  });
}

export function useEvaluateCompliance() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (deviceId: string) => {
      await apiClient.post(`/devices/${deviceId}/evaluate`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['devices'] });
      queryClient.invalidateQueries({ queryKey: ['device'] });
      queryClient.invalidateQueries({ queryKey: ['dashboard'] });
    },
  });
}

// Compliance Rules
export function useComplianceRules(includeDisabled = false) {
  return useQuery({
    queryKey: ['compliance-rules', includeDisabled],
    queryFn: async () => {
      const response = await apiClient.get<ComplianceRuleDto[]>('/compliance-rules', {
        params: { includeDisabled },
      });
      return response.data;
    },
  });
}

export function useCreateComplianceRule() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (rule: Omit<ComplianceRuleDto, 'id' | 'createdAt' | 'updatedAt'>) => {
      const response = await apiClient.post<string>('/compliance-rules', rule);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['compliance-rules'] });
    },
  });
}

export function useUpdateComplianceRule() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (rule: ComplianceRuleDto) => {
      await apiClient.put(`/compliance-rules/${rule.id}`, rule);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['compliance-rules'] });
    },
  });
}

// Alerts
export function useAlertRecipients() {
  return useQuery({
    queryKey: ['alert-recipients'],
    queryFn: async () => {
      const response = await apiClient.get<AlertRecipientDto[]>('/alerts/recipients');
      return response.data;
    },
  });
}

export function useConfigureAlertRecipients() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (recipients: AlertRecipientDto[]) => {
      await apiClient.post('/alerts/recipients', { recipients });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['alert-recipients'] });
    },
  });
}

export interface GetAlertHistoryParams {
  pageNumber?: number;
  pageSize?: number;
  fromDate?: string;
  toDate?: string;
  deviceId?: string;
}

export function useAlertHistory(params: GetAlertHistoryParams = {}) {
  return useQuery({
    queryKey: ['alert-history', params],
    queryFn: async () => {
      const response = await apiClient.get<PaginatedList<AlertHistoryDto>>('/alerts/history', { params });
      return response.data;
    },
  });
}
