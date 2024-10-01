import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { PokerCardComponent } from './components/poker-card/poker-card.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, PokerCardComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'scrum-poker';
}
