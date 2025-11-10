export interface UserProfileDto {
  id: number;
  name: string;
  email: string;
  profilePictureUrl?: string;
  usedStorage: number;
  storageQuota: number;
  formattedUsed: string;
  formattedQuota: string;
}

export interface ChangePassword{
    currentPassword: string;
    newPassword: string;
}

export interface UpdateProfileModel{
    name: string;
    email: string;
}
