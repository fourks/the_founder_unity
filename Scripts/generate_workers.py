"""
This scripts generate `amount` random workers distributed according to a define
skill level distribution. It then prints out the string representations of the
workers to be imported into The Founder.
"""

import random

mu = 5.
sigma = 5.
iters = 10000
amount = 40

# Load names.
with open('male_names.txt', 'r') as f:
    male_names = [l.replace('\n', '') for l in f.readlines()]
with open('female_names.txt', 'r') as f:
    female_names = [l.replace('\n', '') for l in f.readlines()]
with open('last_names.txt', 'r') as f:
    last_names = [l.replace('\n', '') for l in f.readlines()]


# A skill level just defines a classification of a worker's score.
# E.g. workers with a score < X are categorized as some kind of worker.
# The percent defines what percentage of this skill level we want in
# our final collection of workers.
class SkillLevel():
    def __init__(self, percent, score):
        self.percent = percent
        self.score = score


class Worker():
    attrs = ['happiness',
             'productivity',
             'charisma',
             'creativity',
             'cleverness']

    def __init__(self):
        for attr in self.attrs:
            setattr(self, attr, max(1, int(random.gauss(mu, sigma))))

        # Disproporationate males
        if random.random() < 0.7:
            first_names = male_names
        else:
            first_names = female_names
        self.name = ' '.join([random.choice(first_names),
                              random.choice(last_names)])

        # The score is just the sum of attribute values.
        self.score = sum([getattr(self, attr) for attr in self.attrs])

        # Calculate the min salary for a given score.
        self.min_salary = (self.score - 2) * ((self.score % 10) * 1000 + 10000)

    def __repr__(self):
        return '|'.join([self.name, self.title, str(self.min_salary)] +
                        [str(getattr(self, attr)) for attr in self.attrs])

    @property
    def title(self):
        sorted_attrs = sorted(self.attrs,
                              key=lambda attr: getattr(self, attr),
                              reverse=True)
        top = sorted_attrs[:2]

        if 'creativity' in top and 'cleverness' in top:
            return random.choice(['Frontend Developer',
                                  'Backend Developer',
                                  'Programmer',
                                  'Creative Technologist',
                                  'Interactive Developer',
                                  'UX Designer',
                                  'Software Engineer',
                                  'Hardware Engineer'])

        elif 'cleverness' in top and 'charisma' in top:
            return random.choice(['Business Developer',
                                  'Sales Associate',
                                  'Community Manager',
                                  'Product Manager'])

        elif 'creativity' in top and 'charisma' in top:
            return random.choice(['Creative Director',
                                  'Marketing Associate',
                                  'Public Relations Associate',
                                  'Visual Designer',
                                  'UI Designer',
                                  'Designer',
                                  'Product Manager'])

        elif 'creativity' in top:
            return random.choice(['Visual Designer',
                                  'Designer'])

        elif 'cleverness' in top:
            return random.choice(['Hardware Engineer',
                                  'Software Engineer',
                                  'Developer',
                                  'Researcher'])

        elif 'charisma' in top:
            return random.choice(['Sales Associate',
                                  'Public Relations Associate'])

        return random.choice(['Programmer',
                              'Designer',
                              'MBA'])


skill_levels = {
    'lo':     SkillLevel(0.25, 5.),
    'mid-lo': SkillLevel(0.35, 15.),
    'mid-hi': SkillLevel(0.30, 30.),
    'hi':     SkillLevel(0.10, 60.)
}


workers = [Worker() for i in xrange(iters)]
final = []
for level, sl in skill_levels.items():
    limit = int(sl.percent * amount)
    qualifying = [w for w in workers if w.score <= sl.score]
    random.shuffle(qualifying)
    final += qualifying[:limit]

for w in final:
    print(w)