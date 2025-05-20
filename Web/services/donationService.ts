import apiClient from './apiClient';

export type Donation = {
  id?: string;
  campaignId: string;
  userId?: string;
  amount: number;
  isAnonymous: boolean;
  message?: string;
  paymentStatus?: string;
  transactionId?: string;
  paymentMethod?: string;
  currency: string;
  createdAt?: string;
};

export type PaymentRequest = {
  campaignId: string;
  amount: number;
  currency: string;
  description?: string;
  paymentMethod: {
    type: string;
    tokenId?: string;
    cardNumber?: string;
    expiryMonth?: string;
    expiryYear?: string;
    cvc?: string;
    cardholderName?: string;
  };
  customerInfo?: {
    fullName?: string;
    email?: string;
    phone?: string;
    address?: string;
    city?: string;
    country?: string;
    postalCode?: string;
  };
  isAnonymous: boolean;
  message?: string;
};

export const getDonationsByCampaign = async (campaignId: string): Promise<Donation[]> => {
  const response = await apiClient.get<Donation[]>(`/donations/campaign/${campaignId}`);
  return response.data;
};

export const getDonationsByUser = async (): Promise<Donation[]> => {
  const response = await apiClient.get<Donation[]>('/donations/user');
  return response.data;
};

export const getDonationById = async (id: string): Promise<Donation> => {
  const response = await apiClient.get<Donation>(`/donations/${id}`);
  return response.data;
};

export const createDonation = async (paymentRequest: PaymentRequest): Promise<Donation> => {
  const response = await apiClient.post<Donation>('/donations', paymentRequest);
  return response.data;
};

export const getDonationStatistics = async (campaignId: string): Promise<any> => {
  const response = await apiClient.get(`/donations/campaign/${campaignId}/statistics`);
  return response.data;
};

export const createSubscriptionDonation = async (
  campaignId: string,
  amount: number,
  paymentMethodId: string
): Promise<void> => {
  await apiClient.post('/donations/subscription', {
    campaignId,
    amount,
    paymentMethodId
  });
};

export const cancelSubscriptionDonation = async (subscriptionId: string): Promise<void> => {
  await apiClient.delete(`/donations/subscription/${subscriptionId}`);
};

export const generateDonationReceipt = async (donationId: string): Promise<string> => {
  const response = await apiClient.get(`/donations/${donationId}/receipt`);
  return response.data.receiptUrl;
};