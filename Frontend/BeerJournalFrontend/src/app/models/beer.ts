// Beer — model (defines the shape of beer data used in the app)

export interface Beer {
  beerId: number;
  name: string;
  brand: string | null;
  alcoholPct: number | null;
  category: string | null;
}