import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { environment } from 'src/environments/environment.development';
import { PlayService } from './play.service';

@Component({
  selector: 'app-play',
  templateUrl: './play.component.html',
  styleUrls: ['./play.component.css']
})
export class PlayComponent implements OnInit{
  message: any;

  constructor(private playService: PlayService)
  {

  }
  ngOnInit(): void {
    this.playService.getPlayers().subscribe({
      next: (res: any) => this.message = res.value.message,
      error: err => console.log(err)
    })
  }

}
