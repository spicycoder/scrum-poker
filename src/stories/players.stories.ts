import type { Meta, StoryObj } from '@storybook/angular';
import { PlayersComponent } from '../app/components/players/players.component';

const meta: Meta<PlayersComponent> = {
  title: 'Example/Players',
  component: PlayersComponent,
  tags: ['autodocs'],
};

export default meta;
type Story = StoryObj<PlayersComponent>;

/**
 * For displaying players
 */
export const PlayersAllVoted: Story = {
  args: {
    players: [
      {
        display: 'Peter Parker',
        selected: true,
        value: '5',
      },
      {
        display: 'Tony Stark',
        selected: true,
        value: '5',
      },
      {
        display: 'Bruce Wayne',
        selected: true,
        value: '3',
      },
      {
        display: 'Steve Rogers',
        selected: true,
        value: '1',
      },
      {
        display: 'Bruce Banner',
        selected: true,
        value: '8',
      },
      {
        display: 'Natasha Romanoff',
        selected: true,
        value: '5',
      },
      {
        display: 'Stephen Strange',
        selected: true,
        value: '13',
      },
      {
        display: 'Barry Allen',
        selected: true,
        value: '13',
      },
      {
        display: 'Clark Kent',
        selected: true,
        value: '13',
      },
      {
        display: 'Diana Prince',
        selected: true,
        value: '5',
      },
    ]
  },
};

/**
 * For displaying players
 */
export const PlayersNotEveryoneVoted: Story = {
  args: {
    players: [
      {
        display: 'Peter Parker',
        selected: true,
        value: '5',
      },
      {
        display: 'Tony Stark',
        selected: true,
        value: '5',
      },
      {
        display: 'Bruce Wayne',
        selected: true,
        value: null,
      },
      {
        display: 'Steve Rogers',
        selected: true,
        value: '1',
      },
      {
        display: 'Bruce Banner',
        selected: true,
        value: '8',
      },
      {
        display: 'Natasha Romanoff',
        selected: true,
        value: '5',
      },
      {
        display: 'Stephen Strange',
        selected: true,
        value: '13',
      },
      {
        display: 'Barry Allen',
        selected: true,
        value: null,
      },
      {
        display: 'Clark Kent',
        selected: true,
        value: '13',
      },
      {
        display: 'Diana Prince',
        selected: true,
        value: '5',
      },
    ]
  },
};
