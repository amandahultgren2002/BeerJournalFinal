// Imports — Angular tools, RxJS Observable, and our Beer model
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Beer } from '../models/beer';

// Tells Angular this service is available app-wide via DI
@Injectable({
  providedIn: 'root'
})
export class BeerService {

  // Base URL for all the beer endpoints in the API
  private baseUrl = '/api/Beers';

  constructor(private http: HttpClient) {}

  // GET — fetch the full catalogue of beers
  // Used in log-beer to fill the dropdown
  getBeers(): Observable<Beer[]> {
    return this.http.get<Beer[]>(this.baseUrl);
  }

  // GET — fetch one beer by its id
  getBeer(id: number): Observable<Beer> {
    return this.http.get<Beer>(`${this.baseUrl}/${id}`);
  }

  // POST — add a new beer to the catalogue
  // Used when the user picks "+ Add new beer" in the dropdown
  // Returns the created beer (with its new beerId from the database)
  createBeer(beer: Partial<Beer>): Observable<Beer> {
    return this.http.post<Beer>(this.baseUrl, beer);
  }

  // PUT — update an existing beer in the catalogue
  updateBeer(id: number, beer: Partial<Beer>): Observable<Beer> {
    return this.http.put<Beer>(`${this.baseUrl}/${id}`, beer);
  }

  // DELETE — remove a beer (will fail if any entries still reference it)
  deleteBeer(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}