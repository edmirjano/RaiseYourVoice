import apiClient, { handleApiError } from './apiClient';

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
  receiptUrl?: string;
  donorInformation?: {
    email: string;
    fullName: string;
    address: string;
    city: string;
    state: string;
    country: string;
    postalCode: string;
    phone: string;
    isTaxReceiptRequested: boolean;
  };
  isSubscriptionDonation?: boolean;
  subscriptionId?: string;
  refundedAt?: string;
  refundReason?: string;
  createdAt?: string;
  updatedAt?: string;
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
    state?: string;
    country?: string;
    postalCode?: string;
  };
  isAnonymous: boolean;
  message?: string;
  savePaymentMethod?: boolean;
};

/**
 * Get donations for a campaign
 */
export const getDonationsByCampaign = async (
  campaignId: string, 
  includeAnonymous: boolean = false
): Promise<Donation[]> => {
  try {
    const response = await apiClient.get<Donation[]>(`/donations/campaign/${campaignId}`, {
      params: { includeAnonymous }
    });
    return response.data;
  } catch (error) {
    console.error(`Error fetching donations for campaign ${campaignId}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get donations for the current user
 */
export const getUserDonations = async (): Promise<Donation[]> => {
  try {
    const response = await apiClient.get<Donation[]>('/donations/user');
    return response.data;
  } catch (error) {
    console.error('Error fetching user donations:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get a donation by ID
 */
export const getDonationById = async (id: string): Promise<Donation> => {
  try {
    const response = await apiClient.get<Donation>(`/donations/${id}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching donation ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Create a donation
 */
export const createDonation = async (paymentRequest: PaymentRequest): Promise<Donation> => {
  try {
    const response = await apiClient.post<Donation>('/donations', paymentRequest);
    return response.data;
  } catch (error) {
    console.error('Error creating donation:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Refund a donation (admin/moderator only)
 */
export const refundDonation = async (id: string, reason: string): Promise<void> => {
  try {
    await apiClient.post(`/donations/${id}/refund`, { reason });
  } catch (error) {
    console.error(`Error refunding donation ${id}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get donation statistics for a campaign
 */
export const getDonationStatistics = async (campaignId: string): Promise<any> => {
  try {
    const response = await apiClient.get(`/donations/campaign/${campaignId}/statistics`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching donation statistics for campaign ${campaignId}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Get donation insights (admin only)
 */
export const getDonationInsights = async (
  startDate?: Date,
  endDate?: Date
): Promise<any> => {
  try {
    const params: any = {};
    if (startDate) params.startDate = startDate.toISOString();
    if (endDate) params.endDate = endDate.toISOString();
    
    const response = await apiClient.get('/donations/insights', { params });
    return response.data;
  } catch (error) {
    console.error('Error fetching donation insights:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Generate a donation receipt
 */
export const generateDonationReceipt = async (donationId: string): Promise<string> => {
  try {
    const response = await apiClient.get<{ receiptUrl: string }>(`/donations/${donationId}/receipt`);
    return response.data.receiptUrl;
  } catch (error) {
    console.error(`Error generating receipt for donation ${donationId}:`, error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Create a subscription donation
 */
export const createSubscriptionDonation = async (
  campaignId: string,
  amount: number,
  paymentMethodId: string
): Promise<void> => {
  try {
    await apiClient.post('/donations/subscription', {
      campaignId,
      amount,
      paymentMethodId
    });
  } catch (error) {
    console.error('Error creating subscription donation:', error);
    throw new Error(handleApiError(error));
  }
};

/**
 * Cancel a subscription donation
 */
export const cancelSubscriptionDonation = async (subscriptionId: string): Promise<void> => {
  try {
    await apiClient.delete(`/donations/subscription/${subscriptionId}`);
  } catch (error) {
    console.error(`Error cancelling subscription ${subscriptionId}:`, error);
    throw new Error(handleApiError(error));
  }
};