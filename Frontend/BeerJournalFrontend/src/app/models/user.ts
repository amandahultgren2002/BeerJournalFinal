export interface User {
  userId: number;
  firstName: string;
  lastName: string;
  email: string;
  zipCode: number | null;
  city: string | null;
}