import type { Meta, StoryObj } from '@storybook/angular';
import { PointsComponent } from '../app/components/points/points.component';
import { createSignal } from '@angular/core/primitives/signals';

const meta: Meta<PointsComponent> = {
  title: 'Example/Points',
  component: PointsComponent,
  tags: ['autodocs'],
};

export default meta;
type Story = StoryObj<PointsComponent>;

/**
 * For displaying points, when voted
 */
export const PointsAllVoted: Story = {
  args: {
    points: [
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
 * For displaying points, before everyone voted
 */
export const PointsNotEveryoneVoted: Story = {
  args: {
    show: createSignal(false),
    points: [
      {
        display: '',
        selected: false,
        value: '0'
      },
      {
        display: '',
        selected: false,
        value: '0.5'
      },
      {
        display: '',
        selected: false,
        value: '1'
      },
      {
        display: '',
        selected: false,
        value: '2'
      },
      {
        display: '',
        selected: false,
        value: '3'
      },
      {
        display: '',
        selected: false,
        value: '5'
      },
      {
        display: '',
        selected: false,
        value: '8'
      },
      {
        display: '',
        selected: false,
        value: '13'
      },
      {
        display: '',
        selected: false,
        value: '21'
      },
    ]
  },
};

/**
 * For displaying points, before everyone voted
 */
export const PointsSelectedVote: Story = {
  args: {
    show: createSignal(false),
    points: [
      {
        display: '',
        selected: false,
        value: '0'
      },
      {
        display: '',
        selected: false,
        value: '0.5'
      },
      {
        display: '',
        selected: false,
        value: '1'
      },
      {
        display: '',
        selected: false,
        value: '2'
      },
      {
        display: '',
        selected: false,
        value: '3'
      },
      {
        display: '',
        selected: true,
        value: '5'
      },
      {
        display: '',
        selected: false,
        value: '8'
      },
      {
        display: '',
        selected: false,
        value: '13'
      },
      {
        display: '',
        selected: false,
        value: '21'
      },
    ]
  },
};
