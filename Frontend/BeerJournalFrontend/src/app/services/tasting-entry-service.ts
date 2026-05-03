import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TastingEntry } from '../models/tasting-entry';

@Injectable({
  providedIn: 'root'
})
export class TastingEntryService {

  private baseUrl = '/api/TastingEntries';

  constructor(private http: HttpClient) {}

  // GET all tasting entries for the logged-in user
  // Each entry includes a nested `beer` object (populated server-side via JOIN)
  getEntries(): Observable<TastingEntry[]> {
    return this.http.get<TastingEntry[]>(this.baseUrl);
  }

  // GET one entry by its id
  getEntry(id: number): Observable<TastingEntry> {
    return this.http.get<TastingEntry>(`${this.baseUrl}/${id}`);
  }

  // POST — create a new tasting entry
  // The body should include `beerId` (not the four old fields)
  createEntry(entry: Partial<TastingEntry>): Observable<TastingEntry> {
    return this.http.post<TastingEntry>(this.baseUrl, entry);
  }

  // PUT — update an existing tasting entry
  updateEntry(id: number, entry: Partial<TastingEntry>): Observable<TastingEntry> {
    return this.http.put<TastingEntry>(`${this.baseUrl}/${id}`, entry);
  }

  // DELETE
  deleteEntry(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}