import { Injectable } from '@angular/core';
import { Podcast } from '../shared/models/podcast.model';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { HttpClient, HttpHeaders} from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})

export class PodcastService {
  podcasts: Podcast[];

  constructor(private http: HttpClient) { }

  getPodcasts(): Observable<Podcast[]> {
    return this.http.get<Podcast[]>('/api/Track')
                    .pipe(map(podcasts => podcasts.map(podcast => new Podcast().deserialize(podcast))));
  }
}