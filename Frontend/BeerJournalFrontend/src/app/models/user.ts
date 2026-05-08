// User — model (defines the shape of user data used in the app)

export interface User {
  userId: number;
  firstName: string;
  lastName: string;
  email: string;
  zipCode: number | null;
}