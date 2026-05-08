// TastingEntry — model (defines the shape of tasting entry data used in the app)

import { Beer } from './beer';

export interface TastingEntry {
  entryId: number;
  userId: number;

  // Foreign key — what the client SENDS when creating/updating
  beerId: number;

  // Tasting info
  rating: number | null;
  location: string | null;
  testingDate: string | null;   // ISO date string e.g. "2026-05-02"
  price: number | null;
  notes: string | null;


  // Map coordinates
  latitude: number | null;
  longitude: number | null;

  // Nested beer — populated by backend on GET (via JOIN)
  // Optional because on POST/PUT we only send beerId
  beer?: Beer;
}